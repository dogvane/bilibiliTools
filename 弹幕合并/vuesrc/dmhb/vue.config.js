module.exports = {
    baseUrl: process.env.NODE_ENV === 'production' ?
        '/dmhb' : '/',
    devServer: {
        proxy: 'http://localhost:8853'
    },
    outputDir: '../../wwwroot/dmhb'
}