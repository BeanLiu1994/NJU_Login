using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingSystem
{
    #region TaskHolder定义
    //思路是利用定时器或者触发器封装调用task的运行
    //因为不能多继承了所以只能写成一个实例 如果有要用的话直接按照这样写出来就行
    class TaskHolder
    {
        TaskHolder() { AddHandler(); }
        protected void AddHandler() { LoggingSystem.SystemControl.ReturnDataEvent += LoginInfoDataHandler; }
        ~TaskHolder() { DeleteHandler(); }
        protected void DeleteHandler() { LoggingSystem.SystemControl.ReturnDataEvent -= LoginInfoDataHandler; }

        protected void Run()
        {
            // code here
        }
        protected void LoginInfoDataHandler(Pages PageType, bool Hresult, ReturnData HArgs)
        { 
            // code here
        }
    }
    #endregion
}
