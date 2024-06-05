#!/bin/bash

# Устанавливаем дату сборки в формате DD:MM:YYYY
build_date=$(date +'%d.%m.%Y')

# Путь к файлу конфигурации
config_file="config.json"

# Обновляем дату сборки в JSON файле
jq --arg bd "$build_date" '.buildDate = $bd' "$config_file" > temp.json && mv temp.json "$config_file"

dotnet run