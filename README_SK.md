# Letenky Monitor – kompletný opravený projekt

## Čo obsahuje

- WPF aplikáciu pre Windows,
- pridávanie, úpravu, odstraňovanie a vypínanie letov,
- SQLite databázu,
- tri prednastavené lety do Valencie,
- 2 dospelých a kontrolu ±1 deň,
- nastavenie a test Telegramu,
- manuálnu kontrolu cien,
- GitHub Actions build hotovej Windows aplikácie.

## Ako nahrať projekt

V repozitári zmaž starý obsah a nahraj celý obsah tohto ZIP balíka.

Správna štruktúra:

```text
.github/
  workflows/
    build-windows.yml
LetenkyMonitor/
LetenkyMonitor.sln
README_SK.md
```

## Vytvorenie aplikácie

1. Otvor kartu **Actions**.
2. Vyber **Build Windows EXE**.
3. Klikni **Run workflow**.
4. Po úspešnom dokončení otvor build.
5. Dole v sekcii **Artifacts** stiahni `LetenkyMonitor-Windows`.
6. Rozbaľ celý ZIP.
7. Spusti `LetenkyMonitor.exe`.

Priečinok `pw-browsers` musí zostať vedľa EXE.

## Dôležité obmedzenie

Skyscanner môže automatické načítanie cien zablokovať, zobraziť CAPTCHA alebo zmeniť rozloženie stránky. Program preto cenu označuje ako orientačnú a pri neúspechu nevymýšľa žiadnu hodnotu.
