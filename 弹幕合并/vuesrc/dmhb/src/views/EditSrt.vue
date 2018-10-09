<template>
  <div class="home">
      <h1>编辑字幕</h1>
      <p>{{ srt.fileName }}</p>
  <div class="main">
          <table>
              <tr  v-for="item in srtlines" :key='item.id'>
                  <td v-show="!mobile">
                  {{ item.from}} => {{item.to}}<br> ({{ item.duration}})
                  </td>
                  <td>
                      {{ item.text }}
                      <br>
                      {{ item.trans }}
                  </td>
                  <td>
          <button @click="onUp(item.id)">Up</button> 
            <button @click="onDown(item.id)">Down</button>
            <button @click="onTrans(item.id)">Trans</button>
                  </td>
              </tr>
          </table>
  </div>
  </div>
</template>

<style>

.rightTitle
{
    width:200px;
    text-align:right;
    padding-right:10px;
}
button
{
    margin: 5px;
}

</style>

<script>
import webapi from '../api/webapi.js'

export default {
  components: {
  },
  data() {
    return {
        mobile:false,
        srt:{},
        srtlines:[]
    }
  },
  created(){
      let srtId = this.$route.params.srtId;
      if(this.$route.params.mobile)
        this.mobile = true;

      if(srtId == 0 || srtId == undefined)
      {
          console.log('传参错误');
          this.$router.replace('/');
          return;
      }

    const that = this;
    webapi.getSrt(srtId).then(result=>{
      console.log(result.data);
      if(result.data && result.data.data)
      {
        that.srt = result.data.data;
        that.srtlines = that.srt.battutas;
      }
    });
  },
  methods: {
      onChangeItem(result){
          console.log('onChangeItem');
          const that = this;
        if(result.data && result.data.data && result.data.data.length > 0) {
            result.data.data.forEach(changeItem => {
                for(var i=0 ;i < that.srtlines.length; i++)
                { 
                    let item = that.srtlines[i];
                    if(item.id == changeItem.id){
                        item.trans = changeItem.trans;
                        item.text = changeItem.text;
                        break;
                    }
                }
            });
        }
      },
   onUp(id){
       let srtid = this.$route.params.srtId;
       webapi.srtUp(srtid, id).then(this.onChangeItem);
   },
   onDown(id){
       let srtid = this.$route.params.srtId;
       webapi.srtDown(srtid, id).then(this.onChangeItem);
   },
   onTrans(id){
       let srtid = this.$route.params.srtId;
       webapi.srtTrans(srtid, id).then(this.onChangeItem);
   }
  }
}
</script>
