@echo off
set CSC="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist %CSC% (
    echo csc.exe bulunamadi. Lutfen .NET Framework 4.0'in kurulu oldugundan emin olun.
    pause
    exit /b 1
)

echo Uygulama derleniyor...
%CSC% /target:winexe /win32icon:app.ico /out:MagnetGrabber.exe src\MagnetGrabber.cs

if %ERRORLEVEL% equ 0 (
    echo Derleme basarili! MagnetGrabber.exe olusturuldu.
) else (
    echo Derleme sirasinda hata olustu.
)
pause
