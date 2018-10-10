# bilibili发字幕
b站发送字幕的工具程序

项目通过配置文件来配置发字幕的信息
config.json
{
  "srtfile": "翻译中字英语 (自动生成)When Fast is Faster Than Fastest_fix.srt",
  "avid": "25437639",
  "cookie": "buvid2=3C40C317-8126-4638-BD26-A4C2AF150AA630819infoc; ....",
  "fontSize": 36,
  "mode": 5,
  "pool": 1
}

cookie：从chrome里复制出来
avid: 视频id
fontSize 好像24之上也无效。
mode 1表示普通字幕  4表示底部字幕 5表示顶部
pool 0 表示普通弹幕 1 表示字幕弹幕
