FROM prom/alertmanager:latest

COPY ./config/alertmanager/config.yml /etc/alertmanager/config.yml
COPY --chmod=0755 ./docker/dockerfiles/alertmanager/entrypoint.sh /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]