import Vue from 'vue'
import Router from 'vue-router'
import srtlist from './views/SrtList.vue'
import UpSrtFile from './views/UpSrtFile.vue'
import editsrt from './views/EditSrt'
import transsrt from './views/TransSrt'

Vue.use(Router)

export default new Router({
    routes: [{
            path: '/',
            name: 'srtlist',
            component: srtlist
        }, {
            path: '/srtlist',
            name: 'srtlist',
            component: srtlist
        }, {
            path: '/upfile',
            name: 'upfile',
            component: UpSrtFile,
        },
        {
            path: '/editsrt/:srtId/:mobile',
            name: 'editsrt',
            component: editsrt,
        },
        {
            path: '/transsrt/:srtId',
            name: 'transsrt',
            component: transsrt,
        }, {
            path: '/login',
            name: 'login',
            // route level code-splitting
            // this generates a separate chunk (about.[hash].js) for this route
            // which is lazy-loaded when the route is visited.
            component: () =>
                import ( /* webpackChunkName: "about" */ './views/Login.vue')
        }
    ]
})