<template>
    <div class="home">
        <p class="pl">
            <span class="rightTitle">用户名</span><input Type="text" class="inputbox" v-model="username" />
        </p>
        <p class="pl">
            <span class="rightTitle">密&nbsp;&nbsp;&nbsp;&nbsp;码</span><input Type="password" class="inputbox" v-model="pwd" />
        </p>
        <p class="pl">
            <b-button class="register" variant="outline-primary" type="button" v-on:click="login()" >登陆</b-button>
            <b-button class="register" variant="outline-primary" type="button" v-on:click="register()" >注册</b-button>
            <!-- <router-link to="/register" class="register">注册</router-link> -->
        </p>
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
.register {
  margin: 0px 20px;
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
        login () {
            if (this.username == '' || this.pwd == '') {
                alert('请输入用户名和密码');
                return;
            }
            var param = {
                username: this.username,
                pwd: this.pwd
            };
            axios.post(`/api/account/login`, param,
                {
                }).then(response => {

                    var err = response.data.error_msg;
                    if (err && err.length > 0) {
                        alert(err);
                        return;
                    }

                    console.log('token', response.data.data.token);
                    // 登录成功
                    localStorage.setItem("token", response.data.data.token);
                    this.$router.push({ path: '/srtlist' })
                }).catch(error => {
                    alert(error);
                });
        },
        register(){
            this.$router.push({ path:'/register'});
        }
    }
}
</script>
