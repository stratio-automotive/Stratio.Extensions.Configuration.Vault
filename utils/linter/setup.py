from setuptools import setup, find_packages
from src.__version__ import __version__

setup(
    name="stratio-appsettings-linter",
    version=__version__,
    author="Rafael Couto",
    author_email="rafaelcouto@stratioautomotive.com",
    description="Stratio Appsettings files linter for compatibility with Stratio.Extensions.Configuration.Vault",
    license="LGPL-3.0",
    url="https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault",
    packages=find_packages(exclude=["tests"]),
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: LGPL-3.0",
        "Operating System :: OS Independent",
        "Indented Audience :: Developers"
    ],
    install_requires=[
        'rich',
        'validators'
    ],
    python_requires=">=3.10",
    entry_points={
        'console_scripts': [
            'stratio-appsettings-linter = src.main:main'
        ]
    }
)
