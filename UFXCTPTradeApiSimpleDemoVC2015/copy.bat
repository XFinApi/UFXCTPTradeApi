xcopy ..\XTA_W32\Api\UFXCTP_V3.7.1.10 Release\XTA_W32\Api\UFXCTP_V3.7.1.10 /I /E /Y
copy ..\XTA_W32\Cpp\XFinApi.ITradeApi.dll Release\XFinApi.ITradeApi.dll /Y

xcopy ..\XTA_W32\Api\UFXCTP_V3.7.1.10 Debug\XTA_W32\Api\UFXCTP_V3.7.1.10 /I /E /Y
copy ..\XTA_W32\Cpp\XFinApi.ITradeApid.dll Debug\XFinApi.ITradeApid.dll /Y

pause