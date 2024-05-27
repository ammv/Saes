# СУЭП - Система учёта электронных подписей
SAES - System of according electronic signatures 

# Программная характеристика
## Структура
Клиент-Сервер

## Общие характеристики
ЯП - C#
Платформа - [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Клиентскаяя часть
UI фреймворк - Avalonia UI
Паттерн - MVVM

## Серверная часть
gRPC сервер

## База данных
MS SQL

# Как запустить
## Требования
IDE - Visual Studio 2022 или JetBrains Rider
[SDK .NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Шаги
1. Скачать проект
2. Изменить строку подключения в Saes.Configuration.Configuretion.ConnectionString
3. Выполнить SQL скрипт **1 SAES - Main script.sql**
4. Запустить сервер Saes.GrpcServer
5. Запустить клиент Saes.AvaloniaMvvmClient
