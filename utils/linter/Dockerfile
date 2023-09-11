FROM python:3.10-alpine

ARG PYPI_PACKAGE_VERSION
RUN if [ -z "$PYPI_PACKAGE_VERSION" ]; then echo "PYPI_PACKAGE_VERSION must be set"; exit 1; else exit 0; fi

RUN apk update
RUN apk add bash

WORKDIR /app

ADD dist/*.tar.gz /app

RUN pip install -e vault-appsettings-linter-${PYPI_PACKAGE_VERSION}

ENTRYPOINT []

CMD ["/bin/bash"]