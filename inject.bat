@echo off
Injector %1
del %1
ren output_%1 %1
pause&exit/b
