<template>
    <div class="home">
        <p class="pl">
            <span class="rightTitle">用户名</span><input Type="text" class="inputbox" v-model="username" />
        </p>
        <p class="pl">
            <span class="rightTitle">密码</span><input Type="password" class="inputbox" v-model="pwd" />
        </p>
        <p class="pl">
            <button type="button" v-on:click="register()">注册</button>
        </p>
        <p>注意：请妥善保管好账号和密码，本系统没密码找回功能</p>
    </div>
</template>

<style>
.rightTitle {
  width: 200px;
  text-align: right;
  padding-right: 10px;
}
.inputbox {
  width: 120px;
  text-align: left;
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
    data () {
        return {
            files: new FormData(),
            username: '',
            pwd: ''
        }
    },
    methods: {
        register () {
            if (this.username == '' || this.pwd == '') {
                alert('请输入用户名和密码');
                return;
            }
            var param = {
                username: this.username,
                pwd: this.pwd
            };
            axios.post(`/api/account/register`, param,
                {
                }).then(response => {
                    var err = response.data.error_msg;
                    if (err && err.length > 0) {
                        alert(error);
                        return;
                    }

                    localStorage.setItem("token", response.data.data.token);
                    this.$router.push({ path: '/srtlist' })
                }).catch(error => {
                    console.log(error);
                });
        },
    }
}
</script>
