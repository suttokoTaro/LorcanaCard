import csv
from bs4 import BeautifulSoup

# 入力ファイルと出力ファイルのパス
input_html_path = "inputCards_set2.html"
output_csv_path = "00_output_set2.csv"

# カラー変換マップ
COLOR_MAP = {
    "アンバー": "1_umber",
    "アメジスト": "2_amethyst",
    "エメラルド": "3_emerald",
    "ルビー": "4_ruby",
    "サファイア": "5_sapphire",
    "スティール": "6_steel",
}

# カードタイプ変換マップ（"song"を含むかで処理を分けてもOK）
CARD_TYPE_MAP = {
    "キャラクター": "1_character",
    "アクション": "2_action",
    "アクション・歌": "3_action・song",
    "アイテム": "4_item",
    "ロケーション": "5_location",
}

# レアリティ変換マップ
RARITY_MAP = {
    "コモン": "1_common",
    "アンコモン": "2_uncommon",
    "レア": "3_rare",
    "スーパーレア": "4_superrare",
    "レジェンダリー": "5_legendary",
}

def extract_effect_and_flavor(card_div):
    effect_text = ""
    flavor_text = ""

    # 効果テキスト
    effect_span = card_div.find("span", class_="p-cardlist-modal__table-th-effect")
    if effect_span:
        effect_text = effect_span.get_text().replace("\n", "").replace("\r", "").strip()

    # フレーバーテキスト
    flavor_ths = card_div.find_all("th")
    for th in flavor_ths:
        if "フレーバー" in th.get_text():
            td = th.find_next_sibling("td")
            if td:
                flavor_text = td.get_text().replace("\n", "").replace("\r", "").strip()
            break

    return effect_text, flavor_text

# "-" を 0 に変換するユーティリティ関数
def sanitize_stat(value):
    return 0 if value == "-" else value

def parse_card(card_div, card_id):
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

    # カラー・タイプ・レアリティ変換
    color_raw = info.get("インク", "")
    card_type_raw = info.get("タイプ", "")
    rarity_raw = info.get("レアリティ", "")

    # ↓ここでcardIdの下3桁を0埋めで作成
    id_in_set_series = f"{card_id % 1000:03}"
        # --- name, versionName の取得 ---
    heading = card_div.find_previous("h1", class_="p-cardlist-modal-heading-1")
    if heading:
        name_span = heading.find("span", class_="p-cardlist-modal-heading-1__main")
        version_span = heading.find("span", class_="p-cardlist-modal-heading-1__sub")
        name = name_span.get_text(strip=True) if name_span else ""
        version_name = version_span.get_text(strip=True) if version_span else ""
    else:
        name = ""
        version_name = ""

    card_classification = info.get("クラス", "")
    card_type = CARD_TYPE_MAP.get(card_type_raw, card_type_raw)
    if (card_type == "2_action" and card_classification == "歌"):
        card_type = "3_action・song"

    return {
        "cardId": card_id,
        "setSeries": "02_" + info.get("セット", ""),
        "idInSetSeries": id_in_set_series,
        "icon": "",            # 画像は手動またはUnityでアサイン
        "backIcon": "",
        "name": name,
        "versionName": version_name,
        "color": COLOR_MAP.get(color_raw, color_raw),
        "cost": info.get("コスト", ""),
        "cardType": card_type,
        "classification": card_classification,
        "willpower": sanitize_stat(info.get("意思力", "")),
        "strength": sanitize_stat(info.get("攻撃力", "")),
        "loreValue": sanitize_stat(info.get("ロア値", "")),
        "rarity": RARITY_MAP.get(rarity_raw, rarity_raw),
        "inkwellFlag": 0, #（1 if "インクあり" in info.get("インク", "") else 0,
        "vanillaFlag": 1 if effect_text == "-" else 0,
        "bodyguardFlag": 0,
        "challengerFlag": 0,
        "evasiveFlag": 0,
        "recklessFlag": 0,
        "resistFlag": 0,
        "rushFlag": 0,
        "shiftFlag": 0,
        "singerFlag": 0,
        "singTogetherFlag": 0,
        "supportFlag": 0,
        "wardFlag": 0,
        "effectText": effect_text,
        "flavorText": flavor_text,
    }

def main():

    start_card_id = 2001  # ← ここで初期 cardId を指定

    with open(input_html_path, "r", encoding="utf-8") as f:
        html = f.read()

    soup = BeautifulSoup(html, "html.parser")
    card_divs = soup.find_all("div", class_="p-cardlist-modal__card-text")

    print(f"{len(card_divs)} 枚のカードを検出しました。")

    # cardId を start_card_id から始める
    cards = [parse_card(div, card_id) for card_id, div in enumerate(card_divs, start=start_card_id)]

    fieldnames = list(cards[0].keys())

    with open(output_csv_path, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        for card in cards:
            writer.writerow(card)

    print(f"完了: {output_csv_path} に {len(cards)} 件を書き出しました。")

if __name__ == "__main__":
    main()
