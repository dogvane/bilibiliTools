<template>
  <div class="home">
    <p class="pl">
        <span class="rightTitle">用户名</span><input Type="text" class="inputbox" v-modle="username" />
    </p>
        <p class="pl">
        <span class="rightTitle">密码</span><input Type="password" class="inputbox" />
    </p>
    <p class="pl">
        <button type="button" v-on:click="login()">登陆</button>
    </p>
  </div>
</template>

<style>
.rightTitle
{
    width:200px;
    text-align:right;
    padding-right:10px;
}
.inputbox
{
    width:120px;
    text-align:left;
}

</style>

<script>
// @ is an alias to /src
import FileUpload from 'vue-upload-component/dist/vue-upload-component.part.js'
import axios from "axios"

export default {
  components: {
    FileUpload,
  },
  data() {
    return {
      files: new FormData(),
      username:'',
      pwd:''
    }
  },
  methods: {
    login() {
        if(this.username == '' || this.pwd == ''){
            alert('请输入用户名和密码');
            return;
        }
        const files = this.files;
        axios.post(`/Home/UpdateSrtFile`, files,
            {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            }).then(response => {
                alert(response.data);
            }).catch(error => {
                console.log(error);
            });
    },
  }
}
</script>
