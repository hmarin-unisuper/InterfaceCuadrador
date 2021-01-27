@ECHO OFF

START "10S"  /MIN    interfaceCuadrador S 0 0 %1

START "10S"      interfaceCuadrador S 1 10 %1
START "20S"      interfaceCuadrador S 11 20 %1
START "30S"      interfaceCuadrador S 21 30 %1
START "40S"      interfaceCuadrador S 41 50 %1
START "50S"      interfaceCuadrador S 51 60 %1
START "60S"      interfaceCuadrador S 61 70 %1
START "70S"      interfaceCuadrador S 71 80 %1
START "80S"      interfaceCuadrador S 81 90 %1
START "90S"      interfaceCuadrador S 91 100 %1
START "100S"     interfaceCuadrador S 101 110 %1
START "110S"     interfaceCuadrador S 111 120 %1
START "120S"     interfaceCuadrador S 121 130 %1


START "10C"      /MIN interfaceCuadrador  C   1  10 %1
START "20C"      /MIN interfaceCuadrador  C  11  20 %1
START "30C"      /MIN interfaceCuadrador  C  21  30 %1
START "40C"      /MIN interfaceCuadrador  C  31  40 %1
START "50C"      /MIN interfaceCuadrador  C  41  50 %1
START "60C"      /MIN interfaceCuadrador  C  51  60 %1
START "70C"      /MIN interfaceCuadrador  C  61  70 %1
START "80C"      /MIN interfaceCuadrador  C  71  80 %1
START "90C"      /MIN interfaceCuadrador  C  81  90 %1
START "100C"     /MIN interfaceCuadrador  C  91 100 %1
START "110C"     /MIN interfaceCuadrador  C 101 110 %1
START "120C"     /MIN interfaceCuadrador  C 111 120 %1
START "130C"     /MIN interfaceCuadrador  C 121 130 %1

