<template>
  <div class="home">
    <h1>当前字幕</h1>
    <table>
      <tr v-for="srtfile in srtFiles" :key="srtfile.id" class="strlist">
        <td class="title">{{ srtfile.srtFileName }}</td>
        <!-- <div class="time">{{ srtfile.uploadTime + '/' + srtfile.lastUpdate}}</div> -->
        <td class="opBtn">
          <b-dropdown
            class="btnlist"
            variant="info"
            size="sm"
            right
            split
            text="编辑"
            @click="onedit(srtfile.id, false)"
          >
            <b-dropdown-item @click="onedit(srtfile.id, false)">PC编辑</b-dropdown-item>
            <b-dropdown-item @click="onedit(srtfile.id, true)">手机</b-dropdown-item>
          </b-dropdown>

          <b-dropdown class="btnlist" variant="info" size="sm" right text="翻译" split>
            <b-dropdown-item @click="onTrans(srtfile.id, true)">人工翻译字幕</b-dropdown-item>
            <b-dropdown-item @click="onTransAll(srtfile.id)">翻译所有</b-dropdown-item>
          </b-dropdown>

          <b-dropdown
            class="btnlist"
            variant="info"
            size="sm"
            right
            split
            text="删除"
            @click="ondelete(srtfile.id)"
          >
            <b-button class="btnlist" variant="info" size="lg">删除</b-button>
          </b-dropdown>

          <b-dropdown class="btnlist" variant="info" size="sm" right split text="下载">
            <b-dropdown-item @click="ondownload(srtfile.id)">下载英文</b-dropdown-item>
            <b-dropdown-item @click="ondownload2(srtfile.id)">下载中文</b-dropdown-item>
            <b-dropdown-item @click="ondownload3(srtfile.id)">下载中英文</b-dropdown-item>
            <b-dropdown-item @click="ondownload4(srtfile.id)">下载b站弹幕</b-dropdown-item>
          </b-dropdown>
        </td>
      </tr>
    </table>
  </div>
</template>

<style scoped>
.title {
  padding: 0px 10px;
}
.time {
  padding: 0px 10px;
}
/* button {
  padding: 0px 10px;
} */
.btnlist {
  margin: 0px 10px;
}
.strlist {
  line-height: 40px;
  border-bottom-style: solid;
  border-bottom-color: darkgray;
  border-bottom-width: 1px;
}
.opBtn {
  width: 360px;
  float: right;
}
</style>


<script>
import webapi from "../api/webapi.js";
import Cookies from "js-cookie";

export default {
  name: "srtlist",
  components: {},
  data() {
    return {
      srtFiles: []
    };
  },
  created() {
    const token = localStorage.getItem("token");
    Cookies.set("Authorization", token);

    const that = this;
    webapi.getSrtList().then(result => {
      console.log(that);
      if (result.data && result.data.data) {
        that.srtFiles = result.data.data;
      }
    });
  },
  methods: {
    onedit(id, mobile) {
      // this.$router.push({ name: 'editsrt', params: { srtId:id, mobile }})
      this.$router.push({ path: "/editsrt/" + id + "/" + mobile });
    },
    onTrans(id) {
      this.$router.push({ path: "/transsrt/" + id });
    },
    ondelete(id) {
      if (confirm("是否要删除？")) {
        const that = this;
        webapi.deleteSrt(id).then(result => {
          console.log(this);
          if (result.data.error == 0) {
            that.srtFiles = that.srtFiles.filter(o => o.id != id);
          }
        });
      }
    },
    onTransAll(id) {
      webapi.srtTransAll(id).then(result => {
        console.log(result);
        if (result.data.error == 0) {
          alert("已经开始翻译，请稍后刷新");
        } else {
          alert(result.data.error_msg);
        }
      });
    },
    ondownload(id) {
      window.location.href = "../srt/Download/" + id;
    },
    ondownload2(id) {
      window.location.href = "../srt/downloadtrans/" + id;
    },
    ondownload3(id) {
      window.location.href = "../srt/downloadtwolang/" + id;
    },
    ondownload4(id) {
      window.location.href = "../srt/DownloadBilibili/" + id;
    }
  }
};
</script>
