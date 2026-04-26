#!/usr/bin/env python3
"""
Converte o layout histórico da CEF (CSV) para o layout canônico do blob (Contrato V0).

Saída:
{
  "draws": [
    {
      "contest_id": 1,
      "draw_date": "2003-09-29",
      "numbers": [1,2,...,15],
      "winners_15": 0,
      "has_winner_15": false
    },
    ...
  ]
}
"""

from __future__ import annotations

import argparse
import csv
import json
import os
import re
import sys
from dataclasses import dataclass
from datetime import datetime
from typing import Dict, Iterable, List, Optional, Tuple


_NON_ALNUM_RE = re.compile(r"[^a-z0-9]+")


def _norm_header(s: str) -> str:
    # Normalização “boa o suficiente” para CSVs com variações de header.
    # Evita dependências externas (unidecode etc).
    s = (s or "").strip().lower()
    s = s.replace("º", "o").replace("ª", "a")
    s = s.replace("ç", "c")
    s = s.replace("á", "a").replace("à", "a").replace("ã", "a").replace("â", "a")
    s = s.replace("é", "e").replace("ê", "e")
    s = s.replace("í", "i")
    s = s.replace("ó", "o").replace("ô", "o").replace("õ", "o")
    s = s.replace("ú", "u")
    s = _NON_ALNUM_RE.sub("_", s).strip("_")
    return s


def _parse_int(value: str, *, field: str, row_index: int) -> int:
    try:
        return int(str(value).strip())
    except Exception as e:
        raise ValueError(f"linha {row_index}: campo '{field}' inválido para int: {value!r}") from e


def _parse_date_ddmmyyyy(value: str, *, field: str, row_index: int) -> str:
    s = str(value).strip()
    if not s:
        raise ValueError(f"linha {row_index}: campo '{field}' vazio")
    # CEF costuma vir dd/MM/yyyy; normalizamos para yyyy-MM-dd (Contrato V0).
    try:
        dt = datetime.strptime(s, "%d/%m/%Y")
    except ValueError:
        # fallback: alguns arquivos podem vir yyyy-MM-dd já
        try:
            dt = datetime.strptime(s, "%Y-%m-%d")
        except Exception as e:
            raise ValueError(f"linha {row_index}: campo '{field}' inválido para data: {s!r}") from e
    return dt.strftime("%Y-%m-%d")


def _open_text_best_effort(path: str):
    # A CEF frequentemente usa ANSI/Latin-1.
    # Tentamos UTF-8 (com BOM), depois Latin-1.
    try:
        return open(path, "r", encoding="utf-8-sig", newline="")
    except UnicodeDecodeError:
        return open(path, "r", encoding="latin-1", newline="")


def _sniff_dialect(sample: str) -> csv.Dialect:
    # Em arquivos da CEF o separador mais comum é ';'. Para copiar/colar do Excel é comum TAB.
    # O Sniffer às vezes erra com números; aplicamos heurísticas simples antes de delegar.
    semi = sample.count(";")
    comma = sample.count(",")
    tab = sample.count("\t")

    if tab >= max(3, semi + 2, comma + 2):
        class _Tab(csv.Dialect):
            delimiter = "\t"
            quotechar = '"'
            doublequote = True
            skipinitialspace = True
            lineterminator = "\n"
            quoting = csv.QUOTE_MINIMAL

        return _Tab()

    if semi >= max(3, comma + 2, tab + 2):
        class _Semi(csv.Dialect):
            delimiter = ";"
            quotechar = '"'
            doublequote = True
            skipinitialspace = True
            lineterminator = "\n"
            quoting = csv.QUOTE_MINIMAL

        return _Semi()

    if comma >= max(3, semi + 2, tab + 2):
        class _Comma(csv.Dialect):
            delimiter = ","
            quotechar = '"'
            doublequote = True
            skipinitialspace = True
            lineterminator = "\n"
            quoting = csv.QUOTE_MINIMAL

        return _Comma()
    return csv.Sniffer().sniff(sample)


@dataclass(frozen=True)
class _FieldMap:
    contest_id_key: str
    draw_date_key: str
    winners15_key: Optional[str]
    bola_keys: List[str]


def _resolve_field_map(headers: List[str]) -> _FieldMap:
    by_norm: Dict[str, str] = {_norm_header(h): h for h in headers}

    def pick(*candidates: str) -> Optional[str]:
        for c in candidates:
            if c in by_norm:
                return by_norm[c]
        return None

    contest_id = pick("concurso", "numero_concurso", "nr_concurso", "n_concurso", "contest_id")
    if not contest_id:
        raise ValueError(f"não foi possível localizar a coluna do concurso. headers={headers!r}")

    draw_date = pick("data_sorteio", "data_do_sorteio", "dt_sorteio", "draw_date")
    if not draw_date:
        raise ValueError(f"não foi possível localizar a coluna da data do sorteio. headers={headers!r}")

    winners15 = pick(
        "ganhadores_15_acertos",
        "ganhadores_15",
        "qt_ganhadores_15",
        "qtd_ganhadores_15",
        "qtde_ganhadores_15",
        "winners_15",
    )

    # Bolas/dezenas: aceitamos "Bola1..Bola15" e variações com underscore/espaço.
    bola_norm_to_original: List[Tuple[int, str]] = []
    for norm, orig in by_norm.items():
        m = re.fullmatch(r"(?:bola|dezena|d|b)(?:_)?(\d{1,2})", norm)
        if m:
            idx = int(m.group(1))
            if 1 <= idx <= 15:
                bola_norm_to_original.append((idx, orig))

    if len(bola_norm_to_original) < 15:
        # Segundo padrão comum: "bola_01", "bola_02" etc ou "bola 01"
        for norm, orig in by_norm.items():
            m = re.fullmatch(r"(?:bola|dezena)(?:_)?0?(\d{1,2})", norm)
            if m:
                idx = int(m.group(1))
                if 1 <= idx <= 15 and (idx, orig) not in bola_norm_to_original:
                    bola_norm_to_original.append((idx, orig))

    bola_norm_to_original = sorted({idx: orig for idx, orig in bola_norm_to_original}.items())
    bola_keys = [orig for _, orig in bola_norm_to_original if 1 <= _ <= 15]

    if len(bola_keys) != 15:
        raise ValueError(
            "não foi possível localizar 15 colunas de dezenas (Bola1..Bola15). "
            f"encontrei {len(bola_keys)}. headers={headers!r}"
        )

    return _FieldMap(
        contest_id_key=contest_id,
        draw_date_key=draw_date,
        winners15_key=winners15,
        bola_keys=bola_keys,
    )


def _iter_rows(path: str) -> Iterable[Dict[str, str]]:
    with _open_text_best_effort(path) as f:
        sample = f.read(4096)
        f.seek(0)
        dialect = _sniff_dialect(sample)
        # Precisamos suportar: com header e sem header.
        # Estratégia:
        # - Ler a primeira linha como "raw"
        # - Se ela parecer um header (ex.: "Concurso", "Data Sorteio", "Bola1"...), usar DictReader
        # - Se não, assumir layout posicional: concurso, data, bola1..bola15, (opcional winners15)
        reader = csv.reader(f, dialect=dialect)
        try:
            first = next(reader)
        except StopIteration:
            return

        first_norm = [_norm_header(x) for x in first]
        header_markers = {"concurso", "numero_concurso", "data_sorteio", "bola1", "bola_1", "dezena1", "winners_15"}
        looks_like_header = any(x in header_markers for x in first_norm)

        if looks_like_header:
            headers = first
            dict_reader = csv.DictReader(f, dialect=dialect, fieldnames=headers)
            for row in dict_reader:
                # DictReader pode retornar None em chaves quando há colunas a mais.
                yield {k: (v if v is not None else "") for k, v in row.items() if k is not None}
            return

        # Sem header: gerar headers sintéticos por posição.
        # Layout esperado (mínimo): 1 + 1 + 15 = 17 colunas.
        # Layout com winners15: 18 colunas.
        values = first
        col_count = len(values)
        if col_count < 17:
            raise ValueError(
                "arquivo sem header e com poucas colunas. "
                f"esperado >= 17 (concurso, data, 15 dezenas). obtido {col_count}."
            )

        headers = ["Concurso", "Data Sorteio"] + [f"Bola{i}" for i in range(1, 16)]
        if col_count >= 18:
            headers.append("Ganhadores 15 acertos")

        def as_row(vals: List[str]) -> Dict[str, str]:
            # Se tiver mais colunas que o esperado, ignoramos as extras.
            limited = vals[: len(headers)]
            return {headers[i]: (limited[i] if i < len(limited) and limited[i] is not None else "") for i in range(len(headers))}

        yield as_row(values)
        for vals in reader:
            if not vals:
                continue
            yield as_row(vals)


def convert_cef_csv_to_blob_document(path: str) -> Dict[str, List[dict]]:
    rows = list(_iter_rows(path))
    if not rows:
        return {"draws": []}

    field_map = _resolve_field_map(list(rows[0].keys()))
    draws: List[dict] = []

    for i, row in enumerate(rows, start=2):  # header é a linha 1
        if all(str(v).strip() == "" for v in row.values()):
            continue

        contest_id = _parse_int(row[field_map.contest_id_key], field=field_map.contest_id_key, row_index=i)
        draw_date = _parse_date_ddmmyyyy(row[field_map.draw_date_key], field=field_map.draw_date_key, row_index=i)

        numbers = [
            _parse_int(row[k], field=k, row_index=i)
            for k in field_map.bola_keys
        ]
        if len(numbers) != 15:
            raise ValueError(f"linha {i}: esperado 15 dezenas, obtido {len(numbers)}")

        winners_15 = 0
        if field_map.winners15_key:
            winners_15 = _parse_int(row[field_map.winners15_key], field=field_map.winners15_key, row_index=i)

        draws.append(
            {
                "contest_id": contest_id,
                "draw_date": draw_date,
                "numbers": numbers,
                "winners_15": winners_15,
                "has_winner_15": winners_15 > 0,
            }
        )

    draws.sort(key=lambda d: d["contest_id"])
    return {"draws": draws}


def main(argv: Optional[List[str]] = None) -> int:
    p = argparse.ArgumentParser(
        description="Converte CSV histórico (CEF) para o JSON canônico do blob (Contrato V0)."
    )
    p.add_argument("--input", "-i", required=True, help="Caminho para o CSV da CEF (histórico Lotofácil).")
    p.add_argument("--output", "-o", required=True, help="Caminho para gravar o JSON de saída.")
    p.add_argument("--pretty", action="store_true", help="Gera JSON identado (mais legível).")
    args = p.parse_args(argv)

    inp = os.path.abspath(args.input)
    out = os.path.abspath(args.output)

    if not os.path.exists(inp):
        print(f"erro: arquivo de entrada não existe: {inp}", file=sys.stderr)
        return 2
    if not os.path.isfile(inp):
        print(f"erro: entrada não é um arquivo: {inp}", file=sys.stderr)
        return 2

    doc = convert_cef_csv_to_blob_document(inp)
    os.makedirs(os.path.dirname(out), exist_ok=True)

    with open(out, "w", encoding="utf-8", newline="\n") as f:
        if args.pretty:
            json.dump(doc, f, ensure_ascii=False, indent=2, sort_keys=False)
            f.write("\n")
        else:
            json.dump(doc, f, ensure_ascii=False, separators=(",", ":"), sort_keys=False)
            f.write("\n")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

