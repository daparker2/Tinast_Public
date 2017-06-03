# Required PIDs

These are the required PIDs for the display:

  PID   |  Bytes returned | Name | Min Value | Max Value | Unit | Formula | Tick Interval
--------|-----------------|------|-----------|-----------|-------|--------------
  0104	| 1 |  Calculated engine load value	| 0	| 100	|  %	| A*100/255 | 2
  0105	| 1 | 	Engine coolant temperature	| -40	| 215	| °C	| A-40 | 4
  010B	| 1 |	Intake manifold absolute pressure	| 0	| 255 | 	kPa (absolute) |	A | 1
  010F	| 1 |	Intake air temperature	| -40	| 215	| °C |	A-40 | 4
  015C	| 1 |	Engine oil temperature	| -40	| 210	| °C |	A - 40 | 4
  0134 | 1  |     WAFR                   |   0 |    20   |   A/F    | (A*256+B)/32768*14.7 | 1