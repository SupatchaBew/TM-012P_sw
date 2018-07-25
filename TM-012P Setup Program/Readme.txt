-------------------------------------------------
This is software for TM-012P device configulation
-------------------------------------------------


***software current version = 1.1
Please find to-do lists as below
- check if value[16] each value equals to 0
-when connect fail (device timeout, device died, device is not ready to communicate), then it's just die.
-chart button - Read once, read always button -- not create yet
- Device auto connect
- cancel load config file
- program icon
- put current file location in user config
- after clear chart, software can not continue plotting chart
- add sampling time for chart
- nice pop-up warining
- popup for out of range
- lock setting panel when disconnnect

Done fixing bug
-July4-
-can load config file but can not update setting panel( modbus might fail)
-close, minimixe, restore form button -- not create yet
-can not continue reading after click restore default setting. -- click apply after click default

-July23-
-save, load config file dialog does not have select file type yet
- config file can not be loaded, warning
-modbus timeout when write register sometimes. 
-also need status box or pop-up warning when modbus timeout occurs.
-when click connect, we do not have popup warn us to re-connect or to tell what is going on or what to do next
-can not continue reading after click restore default setting. -- Just click default
- block write wrong register, and warining popup