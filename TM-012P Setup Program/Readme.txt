-------------------------------------------------
This is software for TM-012P device configulation
-------------------------------------------------


***software current version = 1.1
Please find to-do lists as below
- check if value[16] each value equals to 0
- when connect fail (device timeout, device died, device is not ready to communicate), then it's just die.
- program icon
- check language correctiveness
- when select wrong device serial port
- load config device with different model
- first time install software, problem with R model check last config


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

-July25-
- refresh search for comport

-July26-
- lock setting panel when disconnnect
- lastest config vs device actual config not match

-July31-
- popup for out of range

-Aug23-
- nice pop-up warining
