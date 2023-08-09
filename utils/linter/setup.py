"""
.NET Projects appsettings Configuration Linter for Stratio Vault Library

Description:
This Python script is a linter that validates the contents of the appsettings.json file(s)
which are related to the Stratio Vault Library.
It ensures that all occurrences of:
 - `{% vault_secret path/to/secret:key %}`
 - `{% vault_dict path/to/secret %}`
 - `{% user_home %}`
 - the Vault JSON object
are consistent with the requirements of the Stratio Vault Library.

Usage:
python -m src.main --work-dir <path_to_the_appsettings_files_folder>
vault-appsettings-linter --work-dir <path_to_the_appsettings_files_folder>

Authors:
Rafael Couto (rafaelcouto@stratioautomotive.com)
Bernardo Marques (bernardomarques@stratioautomotive.com)

License:
This script is licensed under the same license of the parent project.
For more details refer to: https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault/blob/main/License.md
"""

from setuptools import setup, find_packages
from src.__version__ import __version__

setup(
    name="vault-appsettings-linter",
    version=__version__,
    author="Stratio Automotive",
    author_email="devops@stratioautomotive.com",
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
            'vault-appsettings-linter = src.main:main'
        ]
    }
)
