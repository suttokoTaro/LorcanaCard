import csv
from bs4 import BeautifulSoup

# 入力ファイルと出力ファイルのパス
input_html_path = "cards.html"
output_csv_path = "output.csv"

def extract_effect_and_flavor(card_div):
    effect_text = ""
    flavor_text = ""

    # 効果テキスト
    effect_span = card_div.find("span", class_="p-cardlist-modal__table-th-effect")
    if effect_span:
        effect_text = effect_span.get_text(separator="\n").strip()

    # フレーバーテキスト
    flavor_ths = card_div.find_all("th")
    for th in flavor_ths:
        if "フレーバー" in th.get_text():
            td = th.find_next_sibling("td")
            if td:
                flavor_text = td.get_text(separator="\n").strip()
            break

    return effect_text, flavor_text

def parse_card(card_div):
    # 各基本情報取得（例：コスト、インクなど）
    info = {}

    th_td_pairs = card_div.find_all("tr")
    for tr in th_td_pairs:
        th = tr.find("th")
        td = tr.find("td")
        if th and td:
            key = th.get_text(strip=True).replace("\n", "")
            value = td.get_text(separator="\n").strip()
            info[key] = value

    # 効果とフレーバー
    effect_text, flavor_text = extract_effect_and_flavor(card_div)

    return {
        "インク": info.get("インク", ""),
        "コスト": info.get("コスト", ""),
        "攻撃力": info.get("攻撃力", ""),
        "意思力": info.get("意思力", ""),
        "ロア値": info.get("ロア値", ""),
        "タイプ": info.get("タイプ", ""),
        "クラス": info.get("クラス", ""),
        "レアリティ": info.get("レアリティ", ""),
        "セット": info.get("セット", ""),
        "効果テキスト": effect_text,
        "フレーバーテキスト": flavor_text
    }

def main():
    with open(input_html_path, "r", encoding="utf-8") as f:
        html = f.read()

    soup = BeautifulSoup(html, "html.parser")

    card_divs = soup.find_all("div", class_="p-cardlist-modal__card-text")
    print(f"{len(card_divs)} 枚のカードを検出しました。")

    cards = [parse_card(div) for div in card_divs]

    # CSV出力
    fieldnames = [
        "インク", "コスト", "攻撃力", "意思力", "ロア値",
        "タイプ", "クラス", "レアリティ", "セット",
        "効果テキスト", "フレーバーテキスト"
    ]
    with open(output_csv_path, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        for card in cards:
            writer.writerow(card)

    print(f"完了: {output_csv_path} に {len(cards)} 件を書き出しました。")

if __name__ == "__main__":
    main()
