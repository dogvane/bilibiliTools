<template>
  <div id="app">
    <div id="nav">
      <router-link to="/">字幕列表</router-link> |
      <router-link to="/upfile">上传文件</router-link> |
      <router-link to="/login">登陆</router-link>
    </div>
    <router-view/>
  </div>
</template>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
