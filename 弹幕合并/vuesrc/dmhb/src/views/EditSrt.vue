<template>
    <div class="home">
        <h1>编辑字幕</h1>
        <p>{{ srt.fileName }} <a class='skip' :href='lastId'>跳转到上次编辑位置</a></p>
        <div class="main">
            <table class="tbcontent">
                <tr v-for="item in srtlines" :key='item.id' :id='item.id'>
                    <td v-if="mobile === false">
                        {{ item.from.replace('00:', '')}} => {{item.to.replace('00:', '')}}<br> ({{ item.duration.toFixed(2, 10)}})
                    </td>
                    <td v-if="mobile === true">
                        ({{ item.duration.toFixed(2, 10)}})
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
                        <button @click="onLineUp(item.id)">Up Line</button>
                        <button @click="onLineDown(item.id)">Down Line</button>
                    </td>
                </tr>
            </table>
        </div>
        <div class="transbox" :style="mobile?'top:10px':'top:100px'">
            <textarea type="text" class='usertrans' v-model="replaceSource" @blur="onTotalSource()" />
            <textarea type="text" class='usertrans' v-model="replaceData" />
            <button  v-bind:disabled = 'replacing' @click="onReplace()">替换{{ includeCount > 0 ? '(' + includeCount +')':''}}</button>
        </div>
    </div>
</template>

<style>
.skip
{
    padding-left:20px;
    font-size: 12px;
}

.transbox {
  width: 400px;
  /*height: 300px;*/
  position: fixed;
  right: 40px;
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
.usertrans {
  width: 160px;
  height: 2em;
  margin: 5px;
}
</style>

<script>
import webapi from '../api/webapi.js'

let lodash = require('lodash')

export default {
    components: {
    },
    data () {
        return {
            mobile: false,
            lastId: '#1',
            srt: {},
            srtlines: [],
            transids: [],
            replaceSource: '',
            replaceData: '',
            includeCount: 0,
            replacing: false
        }
    },
    created () {
        console.log('on edit', this.$route.params);

        let srtId = this.$route.params.srtId;
        if (this.$route.params.mobile == "true")
            this.mobile = true;

        if (srtId == 0 || srtId == undefined) {
            console.log('传参错误');
            this.$router.replace('/');
            return;
        }

        var lastId = localStorage.getItem(srtId)
        if(lastId){
            this.lastId = '#' + lastId;
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
            console.log('onChangeItem');
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
            console.log('onChangeItem');
            const that = this;
            if (result.data && result.data.data && result.data.data.length > 0) {
                result.data.data.forEach(changeItem => {
                    that.transids.push(changeItem.id);
                });
            }
        },
        onUp (id) {
            let srtid = this.$route.params.srtId;
            //this.transids.push(id);
            lodash.delay(this.onTrans2, 15000);
            webapi.srtUp(srtid, id).then(this.onChangeItem).then(this.pushTransIds);
            localStorage.setItem(srtid, id);
        },
        onDown (id) {
            let srtid = this.$route.params.srtId;
            //this.transids.push(id);
            lodash.delay(this.onTrans2, 15000);
            webapi.srtDown(srtid, id).then(this.onChangeItem).then(this.pushTransIds);
            localStorage.setItem(srtid, id);
        },
        onLineUp (id) {
            let srtid = this.$route.params.srtId;
            //this.transids.push(id);
            // lodash.delay(this.onTrans2, 15000);
            webapi.srtLineUp(srtid, id).then(this.onChangeItem).then(this.pushTransIds);
            localStorage.setItem(srtid, id);
        },
        onLineDown (id) {
            console.log('onLineDown');
            let srtid = this.$route.params.srtId;
            //this.transids.push(id);
            // lodash.delay(this.onTrans2, 15000);
            webapi.srtLineDown(srtid, id).then(this.onChangeItem).then(this.pushTransIds);
            localStorage.setItem(srtid, id);
        },
        onTrans (id) {
            let srtid = this.$route.params.srtId;
            webapi.srtTrans(srtid, id).then(this.onChangeItem);
            localStorage.setItem(srtid, id);
        },
        onTrans2 () {
            let ids = this.transids;
            let srtid = this.$route.params.srtId;
            this.transids = [];
            if (ids.length > 0) {
                webapi.srtTrans2(srtid, ids).then(this.onChangeItem);
            }
        },
        onTotalSource () {
            var includes = this.srtlines.filter(o => o.text.includes(this.replaceSource));
            this.includeCount = includes.length;
        },
        onReplace () {
            let srtid = this.$route.params.srtId;
            if (this.replaceSource.length > 0 && this.replaceData.length > 0) {
                this.replacing = true;
                webapi.replaceSource(srtid, this.replaceSource, this.replaceData)
                    .then(this.onChangeItem).then(() => {
                        this.replaceSource = '';
                        this.replaceData = '';
                        this.includeCount = 0;
                        this.replacing = false;
                    });
            }
        }
    }
}
</script>
