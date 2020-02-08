import Vue from 'vue'
// import Vant from 'vant';
import App from './App.vue'
import router from './router'
// import 'vant/lib/index.css';
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
// custom.scss
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'


Vue.config.productionTip = false

// Install BootstrapVue
Vue.use(BootstrapVue)
// Optionally install the BootstrapVue icon components plugin
Vue.use(IconsPlugin)

// Vue.use(Vant);

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')
