#!/bin/sh
set -e

sed -i "s|\${SMTP_SMARTHOST}|${ALMGR_SMTP_SMARTHOST}|g" /etc/alertmanager/config.yml
sed -i "s|\${SMTP_FROM}|${ALMGR_SMTP_FROM}|g" /etc/alertmanager/config.yml
sed -i "s|\${SMTP_AUTH_IDENTITY}|${ALMGR_SMTP_AUTH_IDENTITY}|g" /etc/alertmanager/config.yml
sed -i "s|\${SMTP_AUTH_USERNAME}|${ALMGR_SMTP_AUTH_USERNAME}|g" /etc/alertmanager/config.yml
sed -i "s|\${SMTP_AUTH_PASSWORD}|${ALMGR_SMTP_AUTH_PASSWORD}|g" /etc/alertmanager/config.yml
sed -i "s|\${EMAIL_ALERT_RECEIVER}|${ALMGR_EMAIL_ALERT_RECEIVER}|g" /etc/alertmanager/config.yml

exec /bin/alertmanager --config.file=/etc/alertmanager/config.yml --storage.path=/etc/alertmanager/data --log.level=debug