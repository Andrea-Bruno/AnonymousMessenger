@echo off

cd %1
FOR /d /r %%F IN (obj?) DO (
    echo deleting folder: %%F
    @IF EXIST %%F RMDIR /S /Q %%F
)

FOR /d /r %%F IN (bin?) DO (
    echo deleting folder: %%F
    @IF EXIST %%F RMDIR /S /Q %%F
)