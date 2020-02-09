<template>
  <div class="home">
    上传文件
        <input type="file" name="updateFile" id="updateFile" multiple=""  v-on:change="fileChange($event.target.files)"/>
        <br>
        <br>
        <input type="checkbox" v-model="ismerge" /> 重建弹幕时间轴
        <button class="btnUpload" type="button" v-on:click="upload()">上传</button>
  </div>
</template>

<style>
/*
import '~vue-upload-component/dist/vue-upload-component.part.css'
@import "~vue-upload-component/dist/vue-upload-component.part.css";

或


 */

 .home
 {
   padding: 10px;
 }
 .btnUpload
 {
   margin: 10px;
 }
.file-uploads {
  overflow: hidden;
  position: relative;
  text-align: center;
  display: inline-block;
}
.file-uploads.file-uploads-html4 input[type="file"] {
  opacity: 0;
  font-size: 20em;
  z-index: 1;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  width: 100%;
  height: 100%;
}
.file-uploads.file-uploads-html5 input[type="file"] {
  overflow: hidden;
  position: fixed;
  width: 1px;
  height: 1px;
  z-index: -1;
  opacity: 0;
}
</style>

<script>
// @ is an alias to /src
// localStorage
import FileUpload from 'vue-upload-component/dist/vue-upload-component.part.js'
import axios from "axios"
// import router from './router'

export default {
  components: {
    FileUpload,
  },
  data() {
    return {
      FormData: new FormData(),
      ismerge: true,
    }
  },
  methods: {
    fileChange(fileList) {
        console.log(fileList);
        this.FormData.append("file", fileList[0], fileList[0].name);
    },
    upload() {
        const postData = this.FormData;
        postData.append('ismerge', this.ismerge);
        axios.post(`/api/srt/updatesrtfile`, postData,
            {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            }).then(response => {
              const result = response.data;
              if(result.error && result.error != 0) {
                alert(result.error_msg);
                return;
              }
              // 上传成功则跳转列表
              console.log(this);
              this.$router.replace('/srtlist');
            }).catch(error => {
                console.log(error);
            });
    },
  }
}
</script>
