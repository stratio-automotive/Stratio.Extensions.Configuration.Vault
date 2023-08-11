FROM python:3.10

ARG PYPI_PACKAGE_VERSION
RUN if [ -z "$PYPI_PACKAGE_VERSION" ]; then echo "PYPI_PACKAGE_VERSION must be set"; exit 1; else exit 0; fi

WORKDIR /app

RUN pip install vault-appsettings-linter==$PYPI_PACKAGE_VERSION

ENTRYPOINT []

CMD ["/bin/bash"]