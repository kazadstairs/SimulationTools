@echo off
set /p name="Enter instancename (including .dotin) to convert: "
SET PATH=%PATH%;C:\Users\Gebruiker\Documents\UU\Software\graphviz-2.38\release\bin
dot -Tpng <%name% >%name%.png
%name%.png