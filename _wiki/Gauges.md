# Gauges and indicators

This is the list of gauges and indicators in cruddy ASCII art, shown at each state.

## Boost
The boost gauge goes from the bottom left of the display to the top right of the display. At 0 psi, the gauge is empty. At 15 psi, it's full. There is a numerical boost indicator on the top of the display which overlaps the progress bar.

### 0 psi
```
  --------- 0 ---------
  |
  |
  | 
  |
  | 
```

### 7 psi

```
  ####----- 7 ---------
  #
  #
  # 
  #
  # 
```

### 15 psi

```
  ######### 7 #########
  #
  #
  # 
  #
  # 
```

## AFR
The AFR gauge is a radial gauge that occupies the center of the display. A progress at the 6 o'clock position of the gauge can swing to the left or right depending on the level of stoichiometry. At 14.7% (stoichiomatic) the gauge is empty at the 6 o'clock position. At 12% (full rich) the gauge will move to the 12 o'clock position on the left. At 16% (full lean) the gauge will move to the 12 o'clock position onthe right. The gauge will stick at 6 o'clock when an idle condition is detected. It will fill both the left and right sides and blink steadily if a lean or rich condition at load. There is a numerical AFR indicator in the middle.

### 14.7%
```
           *
        *     *
       *  14.7 *
        *     *
           #
```

### 11%
```
           #
        #     *
       #  12   *
        #     *
           #
```

### 16%
```
           #
        *     #
       *  16   #
        *     #
           #
```

### Idle
```
           *
        *     *
       *  --   *
        *     *
           *
```

### Full rich/full lean
```
           *              #     
        *     *        #     #
       *  20.2 *  ->  #  20.2 #
        *     *        #     #
           *              #
```

# Oil temp, coolant temp, intake temp
The oil temp, coolant temp, and intake temp indicators on the right side of the display will display the temperature levels as well as blink a warning indicator if one of the temperature levels is outside operating condition, by either being too low or too high.

### Temperature indicator, operating condition
```
     184ºF .
```

### Temperature indicator, outside operating condition
```
     260ºF . -> 260ºF #
```

# CAN connection indicator
If the connection to the scantool is lost, either due to a loss of ECU power or due to a loss of connectivity to the scantool, a CAN connection indicator will begin blinking in the lower right of the display.

# Display fault indicator
If the software or hardware on the display fails, a fault indicator will begin blinking which will (provided the display still works) indicate that the display may no longer be providing accurate data.