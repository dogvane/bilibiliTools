module.exports = {
    baseUrl: process.env.NODE_ENV === 'production' ?
        '/dmhb' : '/',
    devServer: {
        proxy: 'http://localhost:5000'
    },
    outputDir: '../../wwwroot/dmhb'
}