FROM python:3.6-slim-jessie

WORKDIR /script
ENV VERSION="1"

RUN apt-get update && apt-get install -y --no-install-recommends freetds-bin sed libunwind8 libicu52 libssl1.0.0
RUN pip install --upgrade pip
RUN pip install mssql-scripter

CMD ["/bin/sh","-c","mssql-scripter -S localhost -d HISCOM -U sa -P '_fIq}Q#1' --schema-and-data > \"hiscom-v${VERSION}.sql\""]
