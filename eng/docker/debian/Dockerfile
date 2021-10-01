#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Dockerfile that creates a container suitable to build dotnet-cli
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:debian-stretch-20211001171226-047508b

# Install the deb packaging toolchain we need to build debs
RUN apt-get update \
    && apt-get -y install \
        debhelper \
        build-essential \
        devscripts \
        locales \
    && rm -rf /var/lib/apt/lists/*

# liblldb is needed so deb package build does not throw missing library info errors
RUN apt-get update \
    && apt-get -y install liblldb-3.9 \
    && rm -rf /var/lib/apt/lists/*

# Misc Dependencies for build
RUN rm -rf /var/lib/apt/lists/* && \
    apt-get update && \
    apt-get -qqy install \
        sudo && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
    
RUN localedef -c -i en_US -f UTF-8 en_US.UTF-8

# Setup User to match Host User, and give superuser permissions
ARG USER_ID=0
RUN useradd -m code_executor -u ${USER_ID} -g sudo
RUN echo 'code_executor ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers

# With the User Change, we need to change permissions on these directories
RUN chmod -R a+rwx /usr/local
RUN chmod -R a+rwx /home
RUN chmod -R 755 /usr/lib/sudo

# Set user to the one we just created
USER ${USER_ID}

# Set working directory 
ARG WORK_DIR
WORKDIR ${WORK_DIR}
