#!C:\Python27\python.exe
# -*- coding: utf-8 -*-

import cgi
import cgitb;cgitb.enable()
import os
import threading
import codecs

print "Content-type:application/json' \r\n\r\n"
	

def myFunction(username):
	username=str(username)
	message=''
	if(username.startswith('17') and False):
		message+='{#欢迎来到南京大学#}'
		message+='{扫二维码关注公众号}'
		message+='{Image|/bk_wx.png}'
		message+='{Image|/Nanjing_University_Logo2.png}'
	if(username.lower().startswith('mg17') or username.lower().startswith('mf17') or username.lower().startswith('dz17') or username.lower().startswith('dg17')):
		message+='{#欢迎来到南京大学#}'
		message+='{扫二维码关注公众号}'
		message+='{Image|/y_wx.png}'
		message+='{Image|/Nanjing_University_Logo2.png}'
	
	if(username=='00065b8817c5'):
		message+='#南京大学 VISG#'
		message+='{Image|/ImageToday.jpg}'
		message+='http://visgcourse.github.io'
		message+='{}http://visg.nju.edu.cn'
		
	return message
form = cgi.FieldStorage()

try:
	username = form.getvalue('username')
	#username = unicode( username, 'utf8' )
	if username=='':
		message_state=False
		message_chs = 'Username Empty'
	else:
		message_state=True
		message_chs=myFunction(username)
		
	if message_state:
		print '{"Title":"Success","Content":"%s"}'% (message_chs,)
	else:
		print '{"Title":"Error","Content":"%s"}'% (message_chs,)


except KeyError:
	print '{"Title":"Error","Content":"NoUsername"}'
