#!/bin/bash

# Renk tanımlamaları
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color
BOLD='\033[1m'
DIM='\033[2m'

# ASCII Logo
draw_logo() {
    clear
    echo -e "${CYAN}  __  __                       _   ${NC}"
    echo -e "${CYAN} |  \/  | __ _  __ _ _ __   ___| |_ ${NC}"
    echo -e "${CYAN} | |\/| |/ _\` |/ _\` | '_ \ / _ \ __|${NC}"
    echo -e "${CYAN} | |  | | (_| | (_| | | | |  __/ |_ ${NC}"
    echo -e "${CYAN} |_|  |_|\__,_|\__, |_| |_|\___|\__|${NC}"
    echo -e "${CYAN}               |___/                ${NC}"
    echo -e " ${DIM}Stremio Magnet Grabber - macOS Edition${NC}\n"
}

# Mesaj gösterme
print_msg() {
    echo -e "\n  $1\n"
}

# Çıkarım Fonksiyonu
extract_magnet() {
    local INPUT="$1"
    
    if [ -z "$INPUT" ]; then
        print_msg "${RED}✖ Hata: Girdi boş.${NC}"
        return 1
    fi

    # URL Decode işlemi
    local DECODED=$(perl -pe 's/%([0-9a-fA-F]{2})/chr(hex($1))/eg' <<< "$INPUT")

    # Magnet kalıbı arama
    local MAGNET=$(echo "$DECODED" | grep -ioE 'magnet:\?xt=urn:btih:[a-zA-Z0-9]+' | head -n 1)

    if [ -n "$MAGNET" ]; then
        echo -n "$MAGNET" | pbcopy
        print_msg "${GREEN}✔ Başarılı!${NC} Magnet linki panoya kopyalandı:\n  ${CYAN}$MAGNET${NC}"
        return 0
    fi

    # 40 Karakterlik Hex Info Hash Arama
    local HASH=$(echo "$DECODED" | grep -ioE '\b[a-fA-F0-9]{40}\b' | head -n 1)

    if [ -n "$HASH" ]; then
        local HASH_LOWER=$(echo "$HASH" | tr '[:upper:]' '[:lower:]')
        local NEW_MAGNET="magnet:?xt=urn:btih:$HASH_LOWER"
        echo -n "$NEW_MAGNET" | pbcopy
        print_msg "${GREEN}✔ Başarılı!${NC} Yeni Magnet linki oluşturuldu ve panoya kopyalandı:\n  ${CYAN}$NEW_MAGNET${NC}"
        return 0
    fi

    print_msg "${RED}✖ Hata:${NC} Geçerli bir torrent kimliği (info hash) veya magnet linki bulunamadı."
    return 1
}

# Eğer script eskisi gibi "magnet [URL]" şeklinde dışarıdan bir parametreyle çalıştırılırsa, direkt ayıklar ve çıkar.
if [ $# -gt 0 ]; then
    extract_magnet "$1"
    exit $?
fi

# Ana Arayüz Döngüsü
while true; do
    draw_logo
    echo -e " ${CYAN}▶${NC} ${BOLD}1. Panodan Çıkar${NC}    ${DIM}Panodaki Stremio URL'sinden magnet linki al${NC}"
    echo -e " ${CYAN}▶${NC} ${BOLD}2. Manuel Gir${NC}       ${DIM}URL'yi terminale kendin yaz/yapıştır${NC}"
    echo -e " ${CYAN}▶${NC} ${BOLD}Q. Çıkış${NC}            ${DIM}Uygulamayı kapat${NC}"
    echo ""
    echo -e " ${DIM}Seçiminiz (1, 2, Q):${NC} \c"
    
    # Kullanıcıdan Enter'a basmadan tek karakterlik girdi al
    read -r -s -n 1 key
    
    case $key in
        1)
            echo "1" # Basılan tuşu göster
            CONTENT=$(pbpaste)
            if [ -z "$CONTENT" ]; then
                print_msg "${RED}✖ Hata:${NC} Pano boş veya geçersiz metin."
            else
                extract_magnet "$CONTENT"
            fi
            echo -e "  ${DIM}Geri dönmek için herhangi bir tuşa basın...${NC}\c"
            read -r -s -n 1
            ;;
        2)
            echo "2"
            echo -e "\n  ${YELLOW}Lütfen URL'yi yapıştırın ve Enter'a basın: ${NC}\c"
            read -r user_url
            extract_magnet "$user_url"
            echo -e "  ${DIM}Geri dönmek için herhangi bir tuşa basın...${NC}\c"
            read -r -s -n 1
            ;;
        q|Q)
            echo "Q"
            echo -e "\n  ${GREEN}Görüşmek üzere! 👋${NC}\n"
            exit 0
            ;;
        *)
            # Diğer tuşlara basılırsa işlem yapma
            ;;
    esac
done
