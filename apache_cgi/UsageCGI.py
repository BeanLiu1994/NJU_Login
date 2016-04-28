#!C:\Python27\python.exe
# -*- coding: utf-8 -*-

import cgi
import cgitb;cgitb.enable()
import os
import threading
import codecs
import time,datetime

print "Content-type:application/json' \r\n\r\n"
	
form = cgi.FieldStorage()

try:
	username = str(form.getvalue('username'))
	#username = unicode( username, 'utf8' )
	if username=='':
		message_state=False
		message_chs = 'Username Empty'
	else:
		file=open('Users.txt','a')
		timestamp=time.strftime("%Y-%m-%d %X", time.localtime())
		file.write(username+','+timestamp+'\n')
		file.close()
		message_state=True
		message_chs='Okay'
		
	if message_state:
		print '{"Title":"Success","Content":"%s"}'% (message_chs,)
	else:
		print '{"Title":"Error","Content":"%s"}'% (message_chs,)


except KeyError:
	print '{"Title":"Error","Content":"NoUsername"}'
