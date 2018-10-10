::This program will convert all *.dotin files in the current folder to PNG files.
@echo off
SET PATH=%PATH%;C:\Users\Gebruiker\Documents\UU\Software\graphviz-2.38\release\bin
for %%f in (*.dotin) do (
    echo %%~nf
    dot -Tpng <"%%~nf.dotin" >"%%~nf.png"
)
set /p nothing="All .dotin files converted to png. Any key to exit."