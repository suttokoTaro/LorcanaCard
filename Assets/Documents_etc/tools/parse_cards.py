from bs4 import BeautifulSoup
import csv

# HTMLファイルを読み込み
with open("cards.html", "r", encoding="utf-8") as f:
    html = f.read()

soup = BeautifulSoup(html, "html.parser")

# すべてのカードのdivを取得
cards = soup.select("div.p-cardlist-modal__card-text")

card_dicts = []
all_keys = set()

for card in cards:
    data = {}
    for row in card.select("tr"):
        th = row.find("th")
        td = row.find("td")
        if th and td:
            key = th.get_text(strip=True).replace("\n", " ")
            val = td.get_text(strip=True).replace("\n", " ")
            data[key] = val
            all_keys.add(key)
    card_dicts.append(data)

# 一貫した列順で出力するために全てのキーを並べる
sorted_keys = sorted(all_keys)

# CSV書き出し
with open("cards_output.csv", "w", newline="", encoding="utf-8") as f:
    writer = csv.DictWriter(f, fieldnames=sorted_keys)
    writer.writeheader()
    for card in card_dicts:
        writer.writerow(card)

print("✅ CSV出力完了: cards_output.csv")
