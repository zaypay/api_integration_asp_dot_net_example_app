<p align="center">
<img width="200px"src="http://zaypay.com/images/v2/logo.png"/>
</p>
<br>


Doc for Demo App:
---------------------------

The demo application depends on some dlls. All dlls (<i>except zaypay.dll</i>) are located in dlls folder. You can add zaypay dll to the project from  [zaypay plugin] (https://github.com/zaypay/asp_dot_net_zaypay_plugin).

### Port Forwarding

If you want to run this application on your localhost, and yet connect to the Zaypay platform to create payments, receive reports etc, you might need to configure port forwarding on your local machine.

### Some Hints
You have to make sure that your application is accessible from internet. This is important, because Zaypay sends report of your payments to the reporting url you gave when creating your price settings and payalogues at Zaypay.com

If you have a public ip address, you have to make sure that your windows server and iis express allows external connections to your application on a specific port.


Please follow these steps if your application cannot be accessed over the internet:


1. First, you need to tell HTTP.SYS at the kernel level that it's OK to let everyone talk to this URL by making an "Url Reservation." 

  You can check if your url is supported or not by the following httpcfg command:

  ``` 
  httpcfg query urlacl
  ```

  From an administrative command prompt you can make reservation if your url is not supported, for example, port 2167 with any ip address: 

  ```
  httpcfg set urlacl /u http://*:2167/ /a D:(A;;GX;;;WD)
  ```

2. Next, as you want to be able to talk to IIS Express from outside, you need to allow IIS Express through the Windows Firewall. You can do that graphically from Windows, or type in command prompt:

  ```
  netsh firewall add portopening TCP 2167 IISExpressWeb enable ALL
  ```

  where 2167 is the port you want to allow.

3. Finally, you need to change the bindings section in applicationHost.config file of iis express:

  ```
  <binding protocol="http" bindingInformation="*:2167:" />     
  ```


You have to restart iisexpress for changes to take affect.


### Check List

1. Your machine is accessible from external source.
2. __Zaypay.json__ file is in __App_Data__ directory of the project with proper id to key mappings.
3. You have added zaypay dll from zaypay plugin.
4. You have added all dlls in dlls folder
