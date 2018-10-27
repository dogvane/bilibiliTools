<template>
  <div class="home">
    <h1>当前字幕</h1>
    <ul>
      <li v-for="srtfile in srtFiles" :key="srtfile.id">
        <span class="title">{{ srtfile.srtFileName }}</span>
        <span class="time">{{ srtfile.uploadTime + '/' + srtfile.lastUpdate}}</span>
        <span>
          <button @click='onedit(srtfile.id, false)'>编辑</button>
          <button @click='onedit(srtfile.id, true)'>手机编辑</button>
          <button @click='onTrans(srtfile.id, true)'>人工翻译字幕</button>
          <button @click='ondelete(srtfile.id)'>删除</button>
          <button @click='ondownload(srtfile.id)'>下载</button>
          <button @click='ondownload2(srtfile.id)'>下载中文</button>
          <button @click='ondownload3(srtfile.id)'>下载中英文</button>
        </span>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.title{
  padding: 0px 10px;
}
.time{
  padding: 0px 10px;
}
button{
  padding:0px 10px;
}
</style>


<script>
import webapi from '../api/webapi.js'

export default {
  name: 'srtlist',
  components: {
  },data(){
    return {
      srtFiles:[]
    };
  },
  created(){
    
    const that = this;
    webapi.getSrtList().then(result=>{
      console.log(that);
      if(result.data && result.data.data)
      {
        that.srtFiles = result.data.data;
      }
    });
  },
  methods:
  {
    onedit(id, mobile){
      // this.$router.push({ name: 'editsrt', params: { srtId:id, mobile }})
      this.$router.push({ path: '/editsrt/' + id + "/" + mobile})
    },
    onTrans(id){
      this.$router.push({ path: '/transsrt/' + id})
    },
    ondelete(id){
      if(confirm('是否要删除？')){
        const that = this;
        webapi.deleteSrt(id).then(result=>{
          console.log(this);
          if(result.data.error == 0){
            that.srtFiles = that.srtFiles.filter(o=>o.id != id);
          }
        });
      }      
    },
    ondownload(id){
      window.location.href = '../srt/download/' + id;
    },
    ondownload2(id){
      window.location.href = '../srt/downloadtrans/' + id;
    },
    ondownload3(id){
      window.location.href = '../srt/downloadtwolang/' + id;
    }
  }
}
</script>
