import axios from "axios"
import router from './../router'

// 请求拦截器
axios.interceptors.request.use(
    config => {
        // 每次发送请求之前判断是否存在token，如果存在，则统一在http请求的header都加上token，不用每次请求都手动添加了
        // 即使本地存在token，也有可能token是过期的，所以在响应拦截器中要对返回状态进行判断

        const token = localStorage.getItem("token");

        token && (config.headers.Authorization = 'Bearer ' + token);
        return config;
    },
    error => {
        return Promise.error(error);
    })

// 响应拦截器
axios.interceptors.response.use(
    response => {
        if (response.status === 200) {
            return Promise.resolve(response);
        } else {
            return Promise.reject(response);
        }
    },
    // 服务器状态码不是200的情况    
    error => {
        if (error.response.status) {
            switch (error.response.status) {
                // 401: 未登录                
                // 未登录则跳转登录页面，并携带当前页面的路径                
                // 在登录成功后返回当前页面，这一步需要在登录页操作。                
                case 401:
                    router.replace({
                        path: '/login',
                        query: { redirect: router.currentRoute.fullPath }
                    });
                    break;
                    // 403 token过期                
                    // 登录过期对用户进行提示                
                    // 清除本地token和清空vuex中token对象                
                    // 跳转登录页面                
                case 403:
                    // Toast({
                    //     message: '登录过期，请重新登录',
                    //     duration: 1000,
                    //     forbidClick: true
                    // });
                    alert('登录过期，请重新登录');
                    // 清除token                    
                    localStorage.removeItem('token');

                    // 跳转登录页面，并将要浏览的页面fullPath传过去，登录成功后跳转需要访问的页面
                    setTimeout(() => {
                        router.replace({
                            path: '/login',
                            query: {
                                redirect: router.currentRoute.fullPath
                            }
                        });
                    }, 1000);
                    break;
                    // 404请求不存在                
                case 404:
                    // Toast({
                    //     message: '网络请求不存在',
                    //     duration: 1500,
                    //     forbidClick: true
                    // });
                    break;
                    // 其他错误，直接抛出错误提示                
                default:
                    // Toast({
                    //     message: error.response.data.message,
                    //     duration: 1500,
                    //     forbidClick: true
                    // });
                    alert('error.response.data.message');
            }
            return Promise.reject(error.response);
        }
    }
);

const webapi = {
    // 获得字幕列表
    getSrtList() {
        var ret = axios.post(`/api/srt/getsrtlist`);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    getSrt(srtId) {
        var ret = axios.post(`/api/srt/getsrt/?srtId=` + srtId);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtUp(srtId, id) {
        var ret = axios.post(`/api/srt/srtUp/?srtId=` + srtId + "&id=" + id);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtLineUp(srtId, id) {
        // 一次合并一行
        var ret = axios.post(`/api/srt/srtLineUp/?srtId=` + srtId + "&id=" + id);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtDown(srtId, id) {
        var ret = axios.post(`/api/srt/srtDown/?srtId=` + srtId + "&id=" + id);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtLineDown(srtId, id) {
        // 一次合并一行
        var ret = axios.post(`/api/srt/srtLineDown/?srtId=` + srtId + "&id=" + id);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtTrans(srtId, id) {
        var ret = axios.post(`/api/srt/srtTrans/?srtId=` + srtId + "&id=" + id);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    srtTrans2(srtId, id) {
        var ret = axios.post(`/api/srt/srtTrans2/?srtId=` + srtId + "&ids=" + id.join(','));
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    deleteSrt(srtId) {
        var ret = axios.post(`/api/srt/deleteSrt/?srtId=` + srtId);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });

        return ret;
    },
    updateSource(srtId, id, text) {
        var data = { srtId, id, text };
        var ret = axios.post(`/api/srt/updateSource/`, data);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    replaceSource(srtId, text, replace) {
        var data = { srtId, replace, text };
        var ret = axios.post(`/api/srt/replaceSource/`, data);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
    updateTrans(srtId, id, text) {
        var data = { srtId, id, text };
        var ret = axios.post(`/api/srt/updateTrans/`, data);
        ret.then(response => {
            console.log(response.data);
        }).catch(error => {
            console.log(error);
        });
        return ret;
    },
};

export default webapi;