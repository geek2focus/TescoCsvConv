@echo off
for /f "tokens=1-4 delims=/ " %%i in ("%date%") do (
     set dow=%%i
     set day=%%j
     set month=%%k
     set year=%%l
)
set output=%USERPROFILE%\Downloads\Tesco-YNAB-%year%-%month%-%day%.csv
SET /a day=1%day%+1%day%-2%day%
SET /a month=1%month%+1%month%-2%month%
SET /a year=%year%-2000
set input=%USERPROFILE%\Downloads\CreditCard-Transactions-%day%-%month%-%year%.csv
echo In: %input%
echo Out: %output%
TescoCsvConv %input% %output%