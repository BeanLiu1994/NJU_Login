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

def http_get():
    infoname = "E:\\Server\\BingImageInfo.txt"
    if os.path.exists(infoname):
        finfo = open(infoname,"r")
        info=finfo.read()
        finfo.close()
    else:
        info=""
    url='http://lab.dobyi.com/api/bing.php'   #页面的地址
    response = urllib2.urlopen(url)         #调用urllib2向服务器发送get请求
    InfoGet=response.read()
    if InfoGet==info:
        return info
    finfo = open(infoname,"w")
    finfo.write(InfoGet)
    finfo.close()
    mydata = json.loads(InfoGet,object_pairs_hook=OrderedDict)
    response.close()
    f = urllib2.urlopen(mydata["url"]) 
    filetype = mydata["url"].split('.')[-1]
    now=datetime.datetime.now()
    otherStyleTime = now.strftime("%Y_%m_%d_BingImage")
    filename = otherStyleTime + "." + filetype
    if not os.path.exists("E:\\Server\\" + filename):
        with open("E:\\Server\\" + filename, "wb") as code:
            code.write(f.read()) 	
	    f.close()
    mydata["url"]=r"http://visg.nju.edu.cn:16043/" + filename
    mydata["copyrightinfo"]="© 2016 Microsoft"
    return json.dumps(mydata)                    #获取服务器返回的页面信息
    

print "Content-type:application/json' \r\n\r\n"
	
print(http_get())

#print('{"title":"颜值巅峰","desc":"欢迎学弟学妹来305读研读博","url":"http://visg.nju.edu.cn:16043/ImageToday.jpg","copyrightinfo":" "}')
