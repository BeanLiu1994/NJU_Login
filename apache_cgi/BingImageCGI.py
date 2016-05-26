#!C:\Python27\python.exe
# -*- coding: utf-8 -*-

import cgi
import cgitb;cgitb.enable()
import threading
import os
import codecs
import urllib2
import json
from collections import OrderedDict
import datetime

def pic_download(IsDownLoadEnabled):
	url='http://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1'
	response = urllib2.urlopen(url)
	InfoGet=response.read()
	mydata = json.loads(InfoGet,object_pairs_hook=OrderedDict)
	response.close()
	imageurl=mydata['images'][0]['url']
	filetype = imageurl.split('.')[-1]
	now=datetime.datetime.now()
	otherStyleTime = now.strftime("%Y_%m_%d_BingImage")
	filename = otherStyleTime + "." + filetype
	if IsDownLoadEnabled and not os.path.exists("E:\\Server\\" + filename):
		f = urllib2.urlopen(imageurl) 
		with open("E:\\Server\\" + filename, "wb") as code:
			code.write(f.read())
		f.close()
	return r"http://visg.nju.edu.cn:16043/" + filename

def info_comp(InfoGet):
	infoname = "E:\\Server\\BingImageInfo.txt"
	if os.path.exists(infoname):
		finfo = open(infoname,"r")
		info=finfo.read()
		finfo.close()
	else:
		info=""
		
	finfo = open(infoname,"w")
	finfo.write(InfoGet)
	finfo.close()
	
	return info==InfoGet
	
def http_get():
	url='http://cn.bing.com/cnhp/coverstory/'   #页面的地址
	response = urllib2.urlopen(url)         #调用urllib2向服务器发送get请求
	InfoGet=response.read()
	if len(InfoGet)==0:
		return ""
	mydata = json.loads(InfoGet,object_pairs_hook=OrderedDict)
	response.close()
	comp_result=info_comp(InfoGet)
	pic_url=pic_download(not comp_result)
	dataout={'title':mydata['title'],'url':pic_url,'desc':mydata['para1']+mydata['para2'],'copyrightinfo':mydata['provider']+u'  © Microsoft'}
	return json.dumps(dataout)                    #获取服务器返回的页面信息

print "Content-type:application/json' \r\n\r\n"

print(http_get())

#print('{"title":"颜值巅峰","desc":"欢迎学弟学妹来305读研读博","url":"http://visg.nju.edu.cn:16043/ImageToday.jpg","copyrightinfo":" "}')
