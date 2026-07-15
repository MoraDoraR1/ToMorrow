using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources 폴더의 CSV(TextAsset)를 읽어 헤더 기반으로 값을 꺼내 쓰는 간단한 표.
///
/// 헤더는 "지정한 영문 키(headerKey)가 들어 있는 첫 줄"로 찾는다.
/// 그 위에 있는 줄(한글 제목·설명 등)은 몇 줄이든, '#'이 있든 없든,
/// 인코딩이 깨져 있든 전부 무시된다. 덕분에 엑셀에서 제목 줄을 편집해도 안 깨진다.
/// </summary>
public class CsvTable
{
    private readonly List<string[]> rows = new List<string[]>();
    private readonly Dictionary<string, int> columns = new Dictionary<string, int>();

    public int RowCount => rows.Count;

    /// <summary>Resources 경로(확장자 제외)에서 CSV를 읽는다. 실패하면 null.</summary>
    public static CsvTable Load(string resourcePath, string headerKey)
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);
        if (asset == null)
        {
            Debug.LogWarning("CsvTable: CSV를 찾을 수 없습니다 → Resources/" + resourcePath + ".csv");
            return null;
        }

        CsvTable table = Parse(asset.text, headerKey);
        if (table == null)
        {
            Debug.LogWarning("CsvTable: " + resourcePath + ".csv 에서 헤더를 찾지 못했습니다. "
                           + "('" + headerKey + "' 컬럼이 있는 줄이 필요합니다)");
        }
        return table;
    }

    /// <summary>headerKey 컬럼이 있는 줄을 헤더로 삼아 파싱한다. 못 찾으면 null.</summary>
    public static CsvTable Parse(string text, string headerKey)
    {
        if (string.IsNullOrEmpty(text)) return null;

        // 엑셀의 'CSV UTF-8' 저장은 파일 맨 앞에 BOM(U+FEFF)을 붙인다.
        text = text.Replace("\uFEFF", "");

        CsvTable table = new CsvTable();
        bool headerRead = false;

        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim().TrimEnd('\r');
            if (line.Length == 0) continue;

            string[] cells = line.Split(',');
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = cells[i].Trim().Trim('"');
            }

            if (!headerRead)
            {
                // 헤더 줄을 만날 때까지의 모든 줄(한글 제목/설명)은 건너뛴다
                if (Array.IndexOf(cells, headerKey) < 0) continue;

                for (int i = 0; i < cells.Length; i++)
                {
                    if (!string.IsNullOrEmpty(cells[i])) table.columns[cells[i]] = i;
                }
                headerRead = true;
            }
            else
            {
                table.rows.Add(cells);
            }
        }

        return headerRead ? table : null;
    }

    private string Cell(int row, string column)
    {
        if (row < 0 || row >= rows.Count) return null;
        if (!columns.TryGetValue(column, out int c)) return null;
        string[] cells = rows[row];
        if (c >= cells.Length) return null;
        return cells[c];
    }

    public string GetString(int row, string column, string fallback = "")
    {
        string s = Cell(row, column);
        return string.IsNullOrEmpty(s) ? fallback : s;
    }

    public int GetInt(int row, string column, int fallback = 0)
    {
        string s = Cell(row, column);
        return int.TryParse(s, out int v) ? v : fallback;
    }

    public float GetFloat(int row, string column, float fallback = 0f)
    {
        string s = Cell(row, column);
        return float.TryParse(s, out float v) ? v : fallback;
    }

    public bool HasColumn(string column) => columns.ContainsKey(column);
}
