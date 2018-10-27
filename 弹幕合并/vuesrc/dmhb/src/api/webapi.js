import axios from "axios"

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
    srtDown(srtId, id) {
        var ret = axios.post(`/api/srt/srtDown/?srtId=` + srtId + "&id=" + id);
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