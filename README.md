# ocean
## 功能
串口上位机
## 编写方式
基于WPF制作，界面基于mahapps库
## 说明
该工程是fruit工程的WPF版本
对winform的fruit不再更新
不定期会迁移各个界面内的组件为mvvm模式
目前支持串口、以太网的通讯，以及部分TTL转CAN模块
实现Modbus RTU/TCP协议，CAN的DBC报文解析
目前主要考虑上位机的通讯，因此暂时不考虑支持其他通讯