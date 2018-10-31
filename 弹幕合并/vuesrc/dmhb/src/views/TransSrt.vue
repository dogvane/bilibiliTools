<template>
    <div class="home">
        <h1>翻译字幕</h1>
        <p>{{ srt.fileName }}</p>
        <div class="main">
            <table class="tbcontent" width='100%'>
                <tr v-for="item in srtlines" :key='item.id'>
                    <td width="50%">
                        <textarea type="text" v-model="item.text" @blur="souceChange(item.id)" @focus="forcusText(item.id)" />
                        <div class='baidutrans'>
                      {{ item.trans }}
                      </div>
                  </td>
                  <td>
                      <textarea type="text" class='usertrans' v-model="item.trans2" @blur="transChange(item.id)"  @focus="forcusTrans(item.id)"/>
                  </td>
              </tr>
          </table>
  </div>
  </div>
</template>

<style>
.baidutrans {
  padding: 0.5em;
}
textarea {
  width: 90%;
  height: 3em;
  text-align: center;
}
.usertrans {
  height: 4em;
}
.rightTitle {
  width: 200px;
  text-align: right;
  padding-right: 10px;
}
button {
  margin: 5px;
}
.tbcontent {
  border-collapse: collapse;
}
.tbcontent tr {
  border-bottom: 1px silver solid;
}
</style>

<script>
import webapi from '../api/webapi.js'
// let lodash = require('lodash')

export default {
    components: {
    },
    data () {
        return {
            mobile: false,
            srt: {},
            srtlines: [],
            transids: [],
        }
    },
    created () {
        console.log('on edit trans srt', this.$route.params);

        let srtId = this.$route.params.srtId;
        if (this.$route.params.mobile)
            this.mobile = true;

        if (srtId == 0 || srtId == undefined) {
            console.log('传参错误');
            this.$router.replace('/');
            return;
        }

        const that = this;
        webapi.getSrt(srtId).then(result => {
            console.log(result.data);
            if (result.data && result.data.data) {
                that.srt = result.data.data;
                that.srtlines = that.srt.battutas;
            }
        });
    },
    methods: {
        onChangeItem (result) {
            const that = this;
            if (result.data && result.data.data && result.data.data.length > 0) {
                result.data.data.forEach(changeItem => {
                    for (var i = 0; i < that.srtlines.length; i++) {
                        let item = that.srtlines[i];
                        if (item.id == changeItem.id) {
                            if (changeItem.text == '') {
                                // 这个是删除
                                that.srtlines.splice(i, 1);
                            }
                            else {
                                that.srtlines.splice(i, 1, changeItem);
                            }
                            break;
                        }
                    }
                });
            }
            return result;
        },
        pushTransIds (result) {
            const that = this;
            if (result.data && result.data.data && result.data.data.length > 0) {
                result.data.data.forEach(changeItem => {
                    that.transids.push(changeItem.id);
                });
            }
        },
        forcusText (id) {
            var line = this.srtlines.filter(o => o.id == id)[0];
            this.oldtxt = line.text;
        },
        souceChange (id) {
            let srtid = this.$route.params.srtId;
            var line = this.srtlines.filter(o => o.id == id)[0];
            if (line.text == this.oldtxt) {
                if (line.trans == null || line.trans == '') {
                    webapi.srtTrans(srtid, id).then(this.onChangeItem);
                }
            }
            else {
                webapi.updateSource(srtid, id, line.text).then(this.onChangeItem);
            }
        },
        forcusTrans (id) {
            var line = this.srtlines.filter(o => o.id == id)[0];
            this.oldtrans = line.trans2;
        },
        transChange (id) {
            let srtid = this.$route.params.srtId;
            var line = this.srtlines.filter(o => o.id == id)[0];
            if (line.trans2 != this.oldtrans) {
                webapi.updateTrans(srtid, id, line.trans2).then(this.onChangeItem);
            }
        },
    }
}
</script>
