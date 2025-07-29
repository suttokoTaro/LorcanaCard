import csv
from bs4 import BeautifulSoup

# === 入力ファイル ===
with open("cards.html", "r", encoding="utf-8") as f:
    html = f.read()

soup = BeautifulSoup(html, "html.parser")
cards = soup.select("div.p-cardlist-modal__card-text")

output_rows = []

# === ヘッダー：Unity用CardEntityに対応 ===
fieldnames = [
    "cardId", "setSeries", "idInSetSeries", "icon", "backIcon", "name", "versionName",
    "color", "cost", "cardType", "classification", "willpower", "strength", "loreValue",
    "rarity", "inkwellFlag", "vanillaFlag", "bodyguardFlag", "challengerFlag", "evasiveFlag",
    "recklessFlag", "resistFlag", "rushFlag", "shiftFlag", "singerFlag", "singTogetherFlag",
    "supportFlag", "wardFlag"
]

for idx, card in enumerate(cards):
    row = {k: "" for k in fieldnames}
    row["cardId"] = str(idx + 1)  # 自動採番（1〜）

    for table in card.select("table"):
        for tr in table.select("tr"):
            th = tr.find("th")
            td = tr.find("td")
            if not th or not td:
                continue
            key = th.get_text(strip=True)
            val = td.get_text(separator=" ", strip=True)

            # HTMLの表記 → CardEntity項目に変換
            if key == "インク":
                row["color"] = val
            elif key == "コスト":
                row["cost"] = val
            elif key == "攻撃力":
                row["strength"] = val
            elif key == "意思力":
                row["willpower"] = val
            elif key == "ロア値":
                row["loreValue"] = val
            elif key == "タイプ":
                row["cardType"] = val
            elif key == "クラス":
                row["classification"] = val
            elif key == "レアリティ":
                row["rarity"] = val
            elif key == "セット":
                row["setSeries"] = val
            elif key == "カード名":
                row["name"] = val
            elif key == "バージョン名":
                row["versionName"] = val

    output_rows.append(row)

# === 書き出し ===
with open("card_entities.csv", "w", newline="", encoding="utf-8-sig") as f:
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(output_rows)

print("✅ Unity用CardEntity CSVを出力しました！ → card_entities.csv")
