#!C:\Python27\python.exe
# -*- coding: utf-8 -*-

import cgi
import cgitb;cgitb.enable()
import os
import threading
import codecs

from random import choice

table = ["绽放最绚烂的笑容，给明天更美的梦。",\
"愿我的临别赠言是一把伞，能为你遮挡征途上的烈日与风雨。",\
"n年飞逝,毕业分别,愿你前程似锦！",\
"又是一年毕业时，大学的同学们就要分别了，愿大家都能够实现自己的理想。",\
"亲爱的朋友请不要难过，离别以后要彼此珍重。",\
"你有涌泉一样的智慧和一双辛勤的手，学友，学友，不管你身在何处，幸运与快乐时刻陪伴着你！",\
"别丢掉那以往的热情，你仍要保存着那真。",\
"在桃李芬芳的七月，收获你灿烂的笑容。伴着夏天的激情，带去我温馨的问候，祝你毕业大吉，早日找到理想的生活！",\
"毕业了，让我们挥手再见，说一声珍重，道一声祝福，感谢彼此曾经的美好回忆，珍藏彼此真挚的友情，愿你前程似锦。",\
"生命的长河中，留不住的是年华和往事，留下来的是梦和回忆。",\
"坚信自己是颗星，穿云破雾亮晶晶。坚信自己是燧石，不怕敲打和曲折，坚信自己是人才，驱散浮云与阴霾。",\
"六月天空晴朗，毕业钟声敲响；不舍可爱的同窗，难忘尊敬的师长，作别熟悉面庞。迈向成功殿堂，踏着前进的曙光，迎接明日的辉煌。",\
"采撷一串串的梦，学校的嬉戏，回想起是那么绚丽；而成长的追逐，竟已一跃而过。世间的尘嚣喧扰，似乎沉寂，但愿我们不忘过往。",\
"你我有各自的轨迹，如流星，能相聚，共步一段旅程，是缘分，但最终将朝着各自方向渐行渐远，是命运，愿你毕业后的未来更幸福！",\
"六月飞扬着你火热的青春；六月镌刻着你真心的友谊；六月收获着你灿烂的笑容；六月装点着你美好的未来。祝你心想事成，一帆风顺！",\
"莫愁前路无知己，天下谁人不识君。",\
"来自五湖四海，奔向天南海北，三个春天匆匆而过，如梦的年龄，充满了激情和欢笑，不要哭泣，待繁花落尽不要忘了互递你我的消息。"]                

print "Content-type:application/json' \r\n\r\n"
	
def myFunction(username):
	username=str(username)
	message=''
	if(username.startswith('12')):
		message+=choice(table)+' '
	if(username=='121180056'):
		message+='不过你是个逗逼....'
	elif(username=='00065b8817c5'):
		VisgTable=[\
		"visg实验室的童鞋们你们好呀",\
		"打开有惊喜http://visg.nju.edu.cn/305.html",\
		"visg实验室的童鞋们你们好呀",\
		"visg实验室的童鞋们你们好呀",\
		"visg实验室的童鞋们你们好呀",\
		"visg实验室的童鞋们你们好呀",\
		"visg实验室的童鞋们你们好呀",\
		"visg实验室颜值高欢乐多大神强，欢迎学弟学妹们来读研读博"]
		message+=choice(VisgTable)
		message+='{Image|/friendship_normal.jpg}'
		message+='友谊的小船有没有翻呢.'
		message+='{Image|/fear3dremastered.jpg}'
	elif(username=='123456789'):
		message+='Greetings~ tester .你好呀 测试者~。（12开头代表12级 他们就要毕业了）'
		message+='{Image|/friendship_normal.jpg}'
		message+='友谊的小船有没有翻呢.'
		message+='{Image|/fear3dremastered.jpg}'
	elif(username=='121180061'):
		message+='{Image|/fear3dremastered.jpg}'
		message+='好好休息多喝热水。'
	elif(username=='121180060'):
		JBTable=[\
		"然而之后我们还是一个宿舍",\
		"新宿舍还有个qwb",\
		"听说新宿舍在12栋?",\
		"12栋离食堂很近呀",\
		"宿舍3人间空间不小",\
		"下学期是不是要做助教了?",\
		"努力科研ing",\
		"然而之后我们还是一个宿舍"]
		message+=choice(JBTable)
		message+='{Image|/fear3dremastered.jpg}'
	elif(username=='121180077'):
		message+='314的小伙伴们你们好呀~'
		message+='{Image|/fear3dremastered.jpg}'
	elif(username=='121180064'):
		message+='最开始多愉快,基友的小船说翻就翻'
	elif(username=='121180065'):
		message+='312的小伙伴们你们好呀~'
	elif(username=='121180066'):
		message+='好好休息多喝热水。'
	elif(username=='121180067'):
		message+='好好休息多喝热水。'
	elif(username=='121180068'):
		message+='好好休息多喝热水。'
	elif(username=='121180073'):
		message+='好好休息多喝热水。'
	elif(username=='121180075'):
		message+='好好休息多喝热水。'
	elif(username=='121180079'):
		message+='好好休息多喝热水。'
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
